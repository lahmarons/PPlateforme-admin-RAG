import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { FormsModule } from '@angular/forms';
import { catchError, firstValueFrom, of } from 'rxjs';
import { ChangeDemandeStatusCommand, DemandeDto, DemandesClient, Statut } from '../../web-api-client';
import { CitoyenDemandesStoreService } from '../../citoyen/services/citoyen-demandes-store.service';

@Component({
  selector: 'app-agent-demandes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ButtonModule, MessageModule, SelectModule, TableModule, TagModule],
  templateUrl: './agent-demandes.component.html'
})
export class AgentDemandesComponent implements OnInit {
  loading = true;
  demandes: DemandeDto[] = [];
  filteredDemandes: DemandeDto[] = [];
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  selectedStatus: number | null = null;

  readonly statusOptions = [
    { label: 'Toutes', value: null },
    { label: 'En attente', value: Statut.EnAttente },
    { label: 'En cours', value: Statut.EnCours },
    { label: 'Traitée', value: Statut.Traitee },
    { label: 'Refusée', value: Statut.Refusee }
  ];

  constructor(
    private readonly demandesClient: DemandesClient,
    private readonly demandesStore: CitoyenDemandesStoreService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const ids = this.demandesStore.demandeIds();

      if (!ids.length) {
        this.infoMessage = 'Aucune demande connue côté frontend. Ajoutez GET /api/Demandes pour lister toutes les demandes.';
        this.demandes = [];
        this.applyFilter();
        return;
      }

      this.demandes = (await Promise.all(
        ids.map((id) => firstValueFrom(this.demandesClient.getDemandeById(id).pipe(catchError(() => of(null)))))
      )).filter(Boolean) as DemandeDto[];

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
