import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { CitoyenSessionService } from '../services/citoyen-session.service';
import { CitoyenDemandesStoreService } from '../services/citoyen-demandes-store.service';
import { CreateDemandeCommand, DemandesClient, ServiceDto, ServicesClient } from '../../web-api-client';

@Component({
  selector: 'app-citoyen-nouvelle-demande',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, CardModule, SelectModule, TextareaModule, ButtonModule, MessageModule],
  templateUrl: './nouvelle-demande.component.html'
})
export class NouvelleDemandeComponent implements OnInit {
  submitting = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  services: ServiceDto[] = [];

  readonly form = this.fb.nonNullable.group({
    serviceId: [0, [Validators.required, Validators.min(1)]],
    description: ['', [Validators.required, Validators.minLength(10)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly servicesClient: ServicesClient,
    private readonly demandesClient: DemandesClient,
    private readonly session: CitoyenSessionService,
    private readonly store: CitoyenDemandesStoreService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    const serviceId = Number(this.route.snapshot.queryParamMap.get('serviceId'));
    if (!Number.isNaN(serviceId) && serviceId > 0) {
      this.form.patchValue({ serviceId });
    }

    this.servicesClient.getServices().subscribe({
      next: (data) => this.services = data || [],
      error: () => this.errorMessage = 'Impossible de charger les services.'
    });
  }

  submit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    const citoyenId = this.session.citoyen()?.id;
    if (!citoyenId) {
      this.errorMessage = 'Veuillez vous connecter avant de créer une demande.';
      return;
    }

    this.submitting = true;
    this.errorMessage = null;
    this.successMessage = null;

    const value = this.form.getRawValue();
    const command = new CreateDemandeCommand({
      citoyenId,
      serviceId: value.serviceId,
      description: value.description
    });

    this.demandesClient.createDemande(command).subscribe({
      next: (newId) => {
        this.store.addDemandeId(newId);
        this.successMessage = 'Demande créée avec succès.';
        this.submitting = false;
        setTimeout(() => this.router.navigate(['/citoyen/demandes']), 600);
      },
      error: () => {
        this.errorMessage = 'Échec de création de la demande.';
        this.submitting = false;
      }
    });
  }
}
