import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ProgressBarModule } from 'primeng/progressbar';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { catchError, firstValueFrom, of } from 'rxjs';
import { AuthService } from '../../shared/auth/auth.service';
import { CitoyenSessionService } from '../services/citoyen-session.service';
import { DemandesClient, DemandeDto } from '../../web-api-client';

@Component({
  selector: 'app-citoyen-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    MessageModule,
    ProgressBarModule,
    TableModule,
    TagModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;

  demandes: DemandeDto[] = [];
  recentDemandes: DemandeDto[] = [];

  // Metrics
  totalDemandes = 0;
  demandesEnAttente = 0;
  demandesEnCours = 0;
  demandesTraitees = 0;
  demandesRefusees = 0;

  // Rates for Progress Bars
  demandePendingRate = 0;
  demandeEnCoursRate = 0;
  demandeProcessedRate = 0;
  demandeRefusedRate = 0;

  // Service Breakdown for Entity Bars
  servicesBreakdown: { nom: string, count: number, rate: number }[] = [];

  constructor(
    public readonly session: CitoyenSessionService,
    private readonly authService: AuthService,
    private readonly demandesClient: DemandesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const currentUser = await firstValueFrom(this.authService.me().pipe(catchError(() => of(null))));

      if (currentUser?.roles?.includes('Citoyen')) {
        this.session.markConnected(currentUser.userName, currentUser.citoyenId ?? null);
        await this.loadDemandesData();
      } else {
        this.session.clear();
      }
    } catch (err) {
      this.errorMessage = 'Impossible de vérifier votre session ou de charger vos données.';
    } finally {
      this.loading = false;
    }
  }

  private async loadDemandesData(): Promise<void> {
    const page = await firstValueFrom(this.demandesClient.getDemandes(1, 100));
    this.demandes = page?.items ?? [];
    
    // Sort recent demands first (assuming higher IDs are newer)
    this.recentDemandes = [...this.demandes]
      .sort((a, b) => (b.id ?? 0) - (a.id ?? 0))
      .slice(0, 5);

    this.calculateStats();
    this.calculateServiceBreakdown();
  }

  private calculateStats(): void {
    this.totalDemandes = this.demandes.length;
    this.demandesEnAttente = this.demandes.filter(d => d.statut === 0).length;
    this.demandesEnCours = this.demandes.filter(d => d.statut === 1).length;
    this.demandesTraitees = this.demandes.filter(d => d.statut === 2).length;
    this.demandesRefusees = this.demandes.filter(d => d.statut === 3).length;

    const total = this.totalDemandes || 1;
    this.demandePendingRate = Math.round((this.demandesEnAttente / total) * 100);
    this.demandeEnCoursRate = Math.round((this.demandesEnCours / total) * 100);
    this.demandeProcessedRate = Math.round((this.demandesTraitees / total) * 100);
    this.demandeRefusedRate = Math.round((this.demandesRefusees / total) * 100);
  }

  private calculateServiceBreakdown(): void {
    const serviceCounts: { [key: string]: number } = {};
    this.demandes.forEach(d => {
      const service = d.serviceNom || 'Sans service';
      serviceCounts[service] = (serviceCounts[service] || 0) + 1;
    });

    const total = this.totalDemandes || 1;
    this.servicesBreakdown = Object.entries(serviceCounts).map(([nom, count]) => {
      return {
        nom,
        count,
        rate: Math.round((count / total) * 100)
      };
    }).sort((a, b) => b.count - a.count);
  }

  getStatusSeverity(statut: number): 'warn' | 'info' | 'success' | 'danger' | 'secondary' {
    switch (statut) {
      case 0:
        return 'warn';
      case 1:
        return 'info';
      case 2:
        return 'success';
      case 3:
        return 'danger';
      default:
        return 'secondary';
    }
  }
}
