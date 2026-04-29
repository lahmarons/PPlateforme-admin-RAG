import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextModule } from 'primeng/inputtext';
import { ServicesClient, ServiceDto, PrestatairesClient, PrestataireDto } from '../../web-api-client';

interface PrestataireWithServices {
  prestataire: PrestataireDto;
  services: ServiceDto[];
}

@Component({
  selector: 'app-citoyen-services',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, MessageModule, ProgressSpinnerModule, InputTextModule],
  templateUrl: './services.component.html'
})
export class ServicesComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  services: ServiceDto[] = [];
  prestataires: PrestataireDto[] = [];
  prestataireServices: PrestataireWithServices[] = [];
  recherche = '';

  constructor(
    private readonly servicesClient: ServicesClient,
    private readonly prestatairesClient: PrestatairesClient
  ) {}

  ngOnInit(): void {
    this.loadServices();
  }

  loadServices(): void {
    this.servicesClient.getServices().subscribe({
      next: (data) => {
        this.services = data || [];
        this.loadPrestataires();
      },
      error: () => {
        this.errorMessage = 'Impossible de charger les services.';
        this.loading = false;
      }
    });
  }

  loadPrestataires(): void {
    this.prestatairesClient.getPrestataires(undefined, undefined).subscribe({
      next: (data) => {
        this.prestataires = data || [];
        this.buildPrestataireServices();
      },
      error: () => {
        this.errorMessage = 'Impossible de charger les prestataires.';
        this.loading = false;
      }
    });
  }

  applyFilter(): void {
    this.buildPrestataireServices();
  }

  clearFilter(): void {
    this.recherche = '';
    this.buildPrestataireServices();
  }

  private buildPrestataireServices(): void {
    const servicesByPrestataireMap = new Map<number | undefined, ServiceDto[]>();

    // Group services by prestataire
    this.services.forEach(service => {
      const prestataireId = service.prestataireId ?? undefined;
      if (!servicesByPrestataireMap.has(prestataireId)) {
        servicesByPrestataireMap.set(prestataireId, []);
      }
      servicesByPrestataireMap.get(prestataireId)!.push(service);
    });

    this.prestataireServices = [];

    // Add services with prestataires
    servicesByPrestataireMap.forEach((services, prestataireId) => {
      if (prestataireId !== undefined) {
        const prestataire = this.prestataires.find(p => p.id === prestataireId);
        if (prestataire) {
          const filtered = this.filterServices(services);
          if (filtered.length > 0) {
            this.prestataireServices.push({
              prestataire,
              services: filtered
            });
          }
        }
      }
    });

    // Add services without prestataire
    const unassignedServices = servicesByPrestataireMap.get(undefined) || [];
    if (unassignedServices.length > 0) {
      const filtered = this.filterServices(unassignedServices);
      if (filtered.length > 0) {
        const dummyPrestataire = new PrestataireDto();
        dummyPrestataire.id = 0;
        dummyPrestataire.nom = 'Services non assignés';
        dummyPrestataire.description = '';
        this.prestataireServices.push({
          prestataire: dummyPrestataire,
          services: filtered
        });
      }
    }

    // Sort by prestataire name
    this.prestataireServices.sort((a, b) => a.prestataire.nom.localeCompare(b.prestataire.nom));
    this.loading = false;
  }

  private filterServices(services: ServiceDto[]): ServiceDto[] {
    if (!this.recherche.trim()) {
      return services;
    }

    const term = this.recherche.trim().toLowerCase();
    return services.filter(
      s => s.nom.toLowerCase().includes(term) || s.description.toLowerCase().includes(term)
    );
  }
}
