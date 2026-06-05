import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { FormsModule } from '@angular/forms';
import { catchError, firstValueFrom, of } from 'rxjs';
import { ChangeDemandeStatusCommand, DemandeDto, DemandesClient, DocumentsClient, Statut } from '../../web-api-client';

@Component({
  selector: 'app-agent-demandes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ButtonModule, MessageModule, SelectModule, TableModule, TagModule, DialogModule],
  templateUrl: './agent-demandes.component.html'
})
export class AgentDemandesComponent implements OnInit {
  loading = true;
  demandes: DemandeDto[] = [];
  filteredDemandes: DemandeDto[] = [];
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  selectedStatus: number | null = null;
  selectedDemande: DemandeDto | null = null;
  detailsVisible = false;
  downloadingDocId: number | null = null;

  readonly statusOptions = [
    { label: 'Toutes', value: null },
    { label: 'En attente', value: Statut.EnAttente },
    { label: 'En cours', value: Statut.EnCours },
    { label: 'Traitée', value: Statut.Traitee },
    { label: 'Refusée', value: Statut.Refusee }
  ];

  constructor(
    private readonly demandesClient: DemandesClient,
    private readonly documentsClient: DocumentsClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const page = await firstValueFrom(this.demandesClient.getDemandes(1, 200).pipe(catchError(() => of(null))));
      this.demandes = page?.items ?? [];

      if (!this.demandes.length) {
        this.infoMessage = 'Aucune demande à afficher.';
      }

      this.applyFilter();
    } catch {
      this.errorMessage = 'Impossible de charger les demandes.';
    } finally {
      this.loading = false;
    }
  }

  applyFilter(): void {
    if (this.selectedStatus === null || this.selectedStatus === undefined) {
      this.filteredDemandes = [...this.demandes];
      return;
    }

    this.filteredDemandes = this.demandes.filter(d => d.statut === this.selectedStatus);
  }

  openDetails(demande: DemandeDto): void {
    this.selectedDemande = demande;
    this.detailsVisible = true;
  }

  closeDetails(): void {
    this.detailsVisible = false;
    this.selectedDemande = null;
  }

  async downloadDocument(docId: number | undefined, docNom: string, docType: string): Promise<void> {
    if (!docId) return;

    try {
      this.downloadingDocId = docId;
      const document = await firstValueFrom(this.documentsClient.getDocumentById(docId));
      if (document && document.contenu) {
        const byteCharacters = atob(document.contenu);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
          byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: docType || 'application/octet-stream' });
        
        const url = window.URL.createObjectURL(blob);
        const a = window.document.createElement('a');
        a.href = url;
        a.download = docNom;
        window.document.body.appendChild(a);
        a.click();
        window.document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      } else {
        alert('Impossible de récupérer le contenu du document.');
      }
    } catch (err) {
      alert('Erreur lors du téléchargement du document.');
    } finally {
      this.downloadingDocId = null;
    }
  }

  changeStatus(demande: DemandeDto, status: Statut): void {
    if (!demande.id) {
      return;
    }

    const command = new ChangeDemandeStatusCommand({
      demandeId: demande.id,
      newStatus: status
    });

    this.demandesClient.changeDemandeStatus(demande.id, command).subscribe({
      next: () => {
        demande.statut = status;
        demande.statutName = this.getStatusLabel(status);
        this.applyFilter();
      },
      error: () => this.errorMessage = 'Impossible de changer le statut de la demande.'
    });
  }

  private getStatusLabel(status: Statut): string {
    switch (status) {
      case Statut.EnAttente: return 'En attente';
      case Statut.EnCours: return 'En cours';
      case Statut.Traitee: return 'Traitée';
      case Statut.Refusee: return 'Refusée';
      default: return 'Inconnu';
    }
  }
}
