import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { catchError, firstValueFrom, of } from 'rxjs';
import { AuthService } from '../../shared/auth/auth.service';
import { AdminsClient, AgentsClient, DemandesClient, PrestatairesClient, PrestataireDto, ServicesClient } from '../../web-api-client';
import { AdminSessionService } from '../services/admin-session.service';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, CardModule, MessageModule, ProgressBarModule, TagModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  totalAdmins = 0;
  totalAgents = 0;
  activeAgents = 0;
  totalServices = 0;
  activeServices = 0;
  totalDemandes = 0;
  demandesEnAttente = 0;
  demandesTraitees = 0;
  demandesRefusees = 0;
  totalPrestataires = 0;

  // Data lists for reports
  adminsList: any[] = [];
  agentsList: any[] = [];
  servicesList: any[] = [];
  prestatairesList: PrestataireDto[] = [];
  demandesList: any[] = [];

  constructor(
    public readonly session: AdminSessionService,
    private readonly authService: AuthService,
    private readonly adminsClient: AdminsClient,
    private readonly agentsClient: AgentsClient,
    private readonly servicesClient: ServicesClient,
    private readonly demandesClient: DemandesClient,
    private readonly prestatairesClient: PrestatairesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const currentUser = await firstValueFrom(this.authService.me().pipe(catchError(() => of(null))));

      if (!currentUser?.roles?.includes('Administrator')) {
        this.infoMessage = 'Session administrateur non confirmée. Connectez-vous avec un compte administrateur.';
        return;
      }

      this.session.markConnected();

      const [admins, agents, services, demandesPage, prestataires] = await Promise.all([
        firstValueFrom(this.adminsClient.getAdmins()),
        firstValueFrom(this.agentsClient.getAgents()),
        firstValueFrom(this.servicesClient.getServices()),
        firstValueFrom(this.demandesClient.getDemandes(1, 200)),
        firstValueFrom(this.prestatairesClient.getPrestataires(undefined, undefined))
      ]);

      this.adminsList = admins;
      this.agentsList = agents;
      this.servicesList = services;
      this.prestatairesList = prestataires;
      this.totalPrestataires = prestataires.length;

      this.totalAdmins = admins.length;
      this.totalAgents = agents.length;
      this.activeAgents = agents.filter(a => (a as any).isActive !== false).length;
      this.totalServices = services.length;
      this.activeServices = services.filter(s => (s as any).isActive !== false).length;

      const demandes = (demandesPage as any)?.items ?? [];
      this.demandesList = demandes;
      this.totalDemandes = demandes.length;
      this.demandesEnAttente = demandes.filter((d: any) => d.statut === 0 || d.statut === 1).length;
      this.demandesTraitees = demandes.filter((d: any) => d.statut === 2).length;
      this.demandesRefusees = demandes.filter((d: any) => d.statut === 3).length;
    } catch {
      this.errorMessage = 'Impossible de charger les indicateurs du dashboard administrateur.';
    } finally {
      this.loading = false;
    }
  }

  get agentActivationRate(): number {
    return this.totalAgents ? Math.round((this.activeAgents / this.totalAgents) * 100) : 0;
  }

  get serviceActivationRate(): number {
    return this.totalServices ? Math.round((this.activeServices / this.totalServices) * 100) : 0;
  }

  get demandePendingRate(): number {
    return this.totalDemandes ? Math.round((this.demandesEnAttente / this.totalDemandes) * 100) : 0;
  }

  get demandeProcessedRate(): number {
    return this.totalDemandes ? Math.round((this.demandesTraitees / this.totalDemandes) * 100) : 0;
  }

  get demandeRefusedRate(): number {
    return this.totalDemandes ? Math.round((this.demandesRefusees / this.totalDemandes) * 100) : 0;
  }

  get maxEntityCount(): number {
    return Math.max(1, this.totalAdmins, this.totalAgents, this.totalServices, this.totalDemandes);
  }

  entityBarWidth(count: number): string {
    return `${Math.max(8, Math.round((count / this.maxEntityCount) * 100))}%`;
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

  private addPdfHeader(doc: jsPDF, title: string): void {
    // Top primary color accent line
    doc.setFillColor(59, 130, 246); // #3B82F6
    doc.rect(0, 0, 210, 8, 'F'); // A4 size width is 210mm
    
    // Brand title
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(22);
    doc.setTextColor(31, 41, 55); // #1F2937
    doc.text('IdaraTech', 14, 25);
    
    // Subtitle / department
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(107, 114, 128); // #6B7280
    doc.text('Administration & Pilotage', 14, 30);
    
    // Divider line
    doc.setDrawColor(229, 231, 235); // #E5E7EB
    doc.setLineWidth(0.5);
    doc.line(14, 35, 196, 35);
    
    // Report Title
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.setTextColor(17, 24, 39); // #111827
    doc.text(title, 14, 45);
    
    // Generation Metadata
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(156, 163, 175); // #9CA3AF
    const dateStr = new Date().toLocaleString('fr-FR');
    doc.text(`Généré le : ${dateStr}`, 14, 51);
  }

  private addPdfFooter(doc: jsPDF): void {
    const pageCount = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      
      // Divider line
      doc.setDrawColor(229, 231, 235);
      doc.setLineWidth(0.5);
      doc.line(14, 280, 196, 280);
      
      // Footer text
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(8);
      doc.setTextColor(156, 163, 175);
      doc.text('IdaraTech © 2026 - Tous droits réservés', 14, 287);
      doc.text(`Page ${i} sur ${pageCount}`, 180, 287);
    }
  }

  exportRapportGeneral(): void {
    const doc = new jsPDF();
    this.addPdfHeader(doc, "Rapport Général d'Activité");
    
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(12);
    doc.setTextColor(31, 41, 55);
    doc.text("Indicateurs clés du système", 14, 65);
    
    const kpiData = [
      ["Métrique", "Valeur", "Taux / Détails"],
      ["Administrateurs", this.totalAdmins.toString(), "—"],
      ["Agents", this.totalAgents.toString(), `${this.activeAgents} actifs (${this.agentActivationRate}%)`],
      ["Services", this.totalServices.toString(), `${this.activeServices} actifs (${this.serviceActivationRate}%)`],
      ["Prestataires", this.totalPrestataires.toString(), "—"],
      ["Total Demandes", this.totalDemandes.toString(), "—"],
      ["Demandes En Attente/En cours", this.demandesEnAttente.toString(), `${this.demandePendingRate}%`],
      ["Demandes Traitées", this.demandesTraitees.toString(), `${this.demandeProcessedRate}%`],
      ["Demandes Refusées", this.demandesRefusees.toString(), `${this.demandeRefusedRate}%`]
    ];
    
    autoTable(doc, {
      startY: 70,
      head: [kpiData[0]],
      body: kpiData.slice(1),
      theme: 'striped',
      headStyles: { fillColor: [59, 130, 246] },
      styles: { fontSize: 10, cellPadding: 4 },
      columnStyles: {
        0: { fontStyle: 'bold', cellWidth: 80 },
        1: { cellWidth: 40 },
        2: { cellWidth: 60 }
      }
    });

    const finalY = (doc as any).lastAutoTable.finalY + 15;
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(12);
    doc.setTextColor(31, 41, 55);
    doc.text("Observations", 14, finalY);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(55, 65, 81);
    
    const obsLines = [
      `Le service enregistre actuellement un total de ${this.totalDemandes} demandes d'usagers.`,
      `Le taux de traitement global s'élève à ${this.demandeProcessedRate}%, tandis que ${this.demandePendingRate}% des demandes sont en attente de traitement.`,
      `La santé globale des entités est au vert avec un taux d'activation des agents de ${this.agentActivationRate}% et des services de ${this.serviceActivationRate}%.`
    ];
    
    doc.text(obsLines, 14, finalY + 8);
    
    this.addPdfFooter(doc);
    doc.save("rapport_general_idaratech.pdf");
  }

  exportRapportDemandes(): void {
    const doc = new jsPDF();
    this.addPdfHeader(doc, "Rapport Détaillé des Demandes");
    
    const headers = [["ID", "Citoyen", "Service", "Agent Assigné", "Statut", "Description"]];
    const body = this.demandesList.map(d => [
      `#${d.id || ''}`,
      d.citoyenNom || '—',
      d.serviceNom || '—',
      d.agentNom || 'Non assigné',
      d.statutName || this.getStatusText(d.statut),
      d.description || '—'
    ]);
    
    autoTable(doc, {
      startY: 65,
      head: headers,
      body: body,
      theme: 'striped',
      headStyles: { fillColor: [16, 185, 129] },
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { cellWidth: 15, fontStyle: 'bold' },
        1: { cellWidth: 35 },
        2: { cellWidth: 35 },
        3: { cellWidth: 35 },
        4: { cellWidth: 20 },
        5: { cellWidth: 'auto' }
      }
    });
    
    this.addPdfFooter(doc);
    doc.save("rapport_demandes_idaratech.pdf");
  }

  exportRapportAgents(): void {
    const doc = new jsPDF();
    this.addPdfHeader(doc, "Rapport des Agents du Service");
    
    const headers = [["ID", "Nom", "Email", "Matricule", "Grade", "Statut Service", "Service Associé"]];
    const body = this.agentsList.map(a => [
      `#${a.id || ''}`,
      a.nom || '—',
      a.email || '—',
      a.matricule || '—',
      a.grade || '—',
      a.isActive !== false ? 'Actif' : 'Inactif',
      a.serviceNom || '—'
    ]);
    
    autoTable(doc, {
      startY: 65,
      head: headers,
      body: body,
      theme: 'striped',
      headStyles: { fillColor: [14, 165, 233] },
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { cellWidth: 15, fontStyle: 'bold' },
        1: { cellWidth: 30 },
        2: { cellWidth: 40 },
        3: { cellWidth: 20 },
        4: { cellWidth: 20 },
        5: { cellWidth: 20 },
        6: { cellWidth: 'auto' }
      }
    });
    
    this.addPdfFooter(doc);
    doc.save("rapport_agents_idaratech.pdf");
  }

  exportRapportPrestataires(): void {
    const doc = new jsPDF();
    this.addPdfHeader(doc, "Rapport des Prestataires Externes");
    
    const headers = [["ID", "Nom du Prestataire", "Description"]];
    const body = this.prestatairesList.map(p => [
      `#${p.id || ''}`,
      p.nom || '—',
      p.description || '—'
    ]);
    
    autoTable(doc, {
      startY: 65,
      head: headers,
      body: body,
      theme: 'striped',
      headStyles: { fillColor: [139, 92, 246] },
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { cellWidth: 20, fontStyle: 'bold' },
        1: { cellWidth: 50 },
        2: { cellWidth: 'auto' }
      }
    });
    
    this.addPdfFooter(doc);
    doc.save("rapport_prestataires_idaratech.pdf");
  }
}
