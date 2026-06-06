import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { catchError, firstValueFrom, of } from 'rxjs';
import { AuthService } from '../../shared/auth/auth.service';
import { DemandeDto, DemandesClient } from '../../web-api-client';
import { AgentSessionService } from '../services/agent-session.service';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, CardModule, ProgressBarModule, MessageModule, TagModule],
  templateUrl: './agent-dashboard.component.html',
  styleUrl: './agent-dashboard.component.scss'
})
export class AgentDashboardComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  currentAgentName = '';
  
  demandes: DemandeDto[] = [];

  // Counts
  totalDemandes = 0;
  demandesEnAttente = 0;
  demandesEnCours = 0;
  demandesTraitees = 0;
  demandesRefusees = 0;

  // Percentage rates
  demandePendingRate = 0;
  demandeEnCoursRate = 0;
  demandeProcessedRate = 0;
  demandeRefusedRate = 0;

  // Assignment counts
  demandesAssigneesAMoi = 0;
  demandesNonAssignees = 0;
  demandesAssigneesAutres = 0;

  // Assignment rates
  demandesAssigneesAMoiRate = 0;
  demandesNonAssigneesRate = 0;
  demandesAssigneesAutresRate = 0;

  constructor(
    public readonly session: AgentSessionService,
    private readonly authService: AuthService,
    private readonly demandesClient: DemandesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const currentUser = await firstValueFrom(this.authService.me().pipe(catchError(() => of(null))));

      if (!currentUser?.roles?.includes('Agent')) {
        this.infoMessage = 'Session agent non confirmée. Connectez-vous avec un compte agent.';
        return;
      }

      this.currentAgentName = currentUser?.userName || '';
      this.session.markConnected();

      const demandesPage = await firstValueFrom(this.demandesClient.getDemandes(1, 200));
      const demandes = (demandesPage as any)?.items ?? [];
      this.demandes = demandes;
      this.totalDemandes = demandes.length;
      
      this.demandesEnAttente = demandes.filter((d: DemandeDto) => d.statut === 0).length;
      this.demandesEnCours = demandes.filter((d: DemandeDto) => d.statut === 1).length;
      this.demandesTraitees = demandes.filter((d: DemandeDto) => d.statut === 2).length;
      this.demandesRefusees = demandes.filter((d: DemandeDto) => d.statut === 3).length;

      // Rates
      this.demandePendingRate = this.totalDemandes ? Math.round((this.demandesEnAttente / this.totalDemandes) * 100) : 0;
      this.demandeEnCoursRate = this.totalDemandes ? Math.round((this.demandesEnCours / this.totalDemandes) * 100) : 0;
      this.demandeProcessedRate = this.totalDemandes ? Math.round((this.demandesTraitees / this.totalDemandes) * 100) : 0;
      this.demandeRefusedRate = this.totalDemandes ? Math.round((this.demandesRefusees / this.totalDemandes) * 100) : 0;

      // Current agent name
      const currentUserName = currentUser?.userName || '';
      
      this.demandesAssigneesAMoi = demandes.filter((d: DemandeDto) => d.agentNom === currentUserName).length;
      this.demandesNonAssignees = demandes.filter((d: DemandeDto) => !d.agentNom).length;
      this.demandesAssigneesAutres = demandes.filter((d: DemandeDto) => d.agentNom && d.agentNom !== currentUserName).length;

      this.demandesAssigneesAMoiRate = this.totalDemandes ? Math.round((this.demandesAssigneesAMoi / this.totalDemandes) * 100) : 0;
      this.demandesNonAssigneesRate = this.totalDemandes ? Math.round((this.demandesNonAssignees / this.totalDemandes) * 100) : 0;
      this.demandesAssigneesAutresRate = this.totalDemandes ? Math.round((this.demandesAssigneesAutres / this.totalDemandes) * 100) : 0;

      if (!demandes.length) {
        this.infoMessage = 'Aucune demande disponible pour le moment.';
      }
    } catch {
      this.errorMessage = 'Impossible de charger les demandes du tableau agent.';
    } finally {
      this.loading = false;
    }
  }

  statusSeverity(statut: number): 'warn' | 'success' | 'danger' | 'info' {
    switch (statut) {
      case 2:
        return 'success';
      case 3:
        return 'danger';
      case 1:
        return 'info';
      default:
        return 'warn';
    }
  }

  getStatusText(statut: number): string {
    switch (statut) {
      case 0: return 'En attente';
      case 1: return 'En cours';
      case 2: return 'Traitée';
      case 3: return 'Refusée';
      default: return 'Inconnu';
    }
  }

  exportRapportInstantane(): void {
    const doc = new jsPDF();
    
    // Top primary color accent line
    doc.setFillColor(34, 197, 94); // Green accent for Agent #22C55E
    doc.rect(0, 0, 210, 8, 'F');
    
    // Brand title
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(22);
    doc.setTextColor(31, 41, 55);
    doc.text('IdaraTech', 14, 25);
    
    // Subtitle / department
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(107, 114, 128);
    doc.text(`Dashboard Agent • ${this.currentAgentName || 'Agent'}`, 14, 30);
    
    // Divider line
    doc.setDrawColor(229, 231, 235);
    doc.setLineWidth(0.5);
    doc.line(14, 35, 196, 35);
    
    // Report Title
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.setTextColor(17, 24, 39);
    doc.text("Rapport Instantané d'Activité", 14, 45);
    
    // Generation Metadata
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(156, 163, 175);
    const dateStr = new Date().toLocaleString('fr-FR');
    doc.text(`Généré le : ${dateStr}`, 14, 51);
    
    // Section 1: KPI summary table
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(12);
    doc.setTextColor(31, 41, 55);
    doc.text("Résumé de la charge de travail", 14, 65);
    
    const kpiData = [
      ["Statut", "Nombre", "Pourcentage"],
      ["En attente", this.demandesEnAttente.toString(), `${this.demandePendingRate}%`],
      ["En cours", this.demandesEnCours.toString(), `${this.demandeEnCoursRate}%`],
      ["Traitées", this.demandesTraitees.toString(), `${this.demandeProcessedRate}%`],
      ["Refusées", this.demandesRefusees.toString(), `${this.demandeRefusedRate}%`],
      ["Total Demandes Service", this.totalDemandes.toString(), "100%"]
    ];
    
    autoTable(doc, {
      startY: 70,
      head: [kpiData[0]],
      body: kpiData.slice(1),
      theme: 'striped',
      headStyles: { fillColor: [34, 197, 94] },
      styles: { fontSize: 10, cellPadding: 3 },
      columnStyles: {
        0: { fontStyle: 'bold', cellWidth: 80 },
        1: { cellWidth: 50 },
        2: { cellWidth: 50 }
      }
    });
    
    // Section 2: Detailed demands list
    const finalY = (doc as any).lastAutoTable.finalY + 12;
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(12);
    doc.setTextColor(31, 41, 55);
    doc.text("Liste des demandes du service", 14, finalY);
    
    const headers = [["ID", "Citoyen", "Service", "Agent", "Statut"]];
    const body = this.demandes.map(d => [
      `#${d.id || ''}`,
      d.citoyenNom || '—',
      d.serviceNom || '—',
      d.agentNom || 'Non assigné',
      d.statutName || this.getStatusText(d.statut ?? 0)
    ]);
    
    autoTable(doc, {
      startY: finalY + 5,
      head: headers,
      body: body,
      theme: 'striped',
      headStyles: { fillColor: [59, 130, 246] },
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { cellWidth: 20, fontStyle: 'bold' },
        1: { cellWidth: 45 },
        2: { cellWidth: 45 },
        3: { cellWidth: 45 },
        4: { cellWidth: 'auto' }
      }
    });
    
    // Add page numbers
    const pageCount = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.setDrawColor(229, 231, 235);
      doc.setLineWidth(0.5);
      doc.line(14, 280, 196, 280);
      
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(8);
      doc.setTextColor(156, 163, 175);
      doc.text('IdaraTech © 2026 - Tous droits réservés', 14, 287);
      doc.text(`Page ${i} sur ${pageCount}`, 180, 287);
    }
    
    doc.save(`rapport_instantane_${this.currentAgentName || 'agent'}.pdf`);
  }
}
