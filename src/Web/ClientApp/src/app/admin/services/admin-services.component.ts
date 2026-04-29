import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { firstValueFrom } from 'rxjs';
import { CreateServiceCommand, PrestataireDto, PrestatairesClient, ServiceDto, ServicesClient, UpdateServiceCommand } from '../../web-api-client';

@Component({
  selector: 'app-admin-services',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, ButtonModule, CardModule, DialogModule, InputTextModule, MessageModule, SelectModule, TableModule, TagModule],
  templateUrl: './admin-services.component.html'
})
export class AdminServicesComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  services: ServiceDto[] = [];
  prestataires: PrestataireDto[] = [];
  dialogVisible = false;
  editingService: ServiceDto | null = null;
  readonly statusOptions = [
    { label: 'Actif', value: true },
    { label: 'Désactivé', value: false }
  ];

  readonly form = this.fb.nonNullable.group({
    nom: ['', [Validators.required]],
    description: ['', [Validators.required]],
    isActive: [true],
    prestataireId: [null as number | null]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly servicesClient: ServicesClient,
    private readonly prestatairesClient: PrestatairesClient
  ) {}

  async ngOnInit(): Promise<void> {
    await this.loadPrestataires();
    await this.load();
  }

  async loadPrestataires(): Promise<void> {
    try {
      this.prestataires = await firstValueFrom(this.prestatairesClient.getPrestataires(undefined, undefined));
    } catch {
      this.errorMessage = 'Impossible de charger les prestataires.';
    }
  }

  async load(): Promise<void> {
    try {
      this.services = await firstValueFrom(this.servicesClient.getServices());
    } catch {
      this.errorMessage = 'Impossible de charger les services.';
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    this.editingService = null;
    this.form.reset({ nom: '', description: '', isActive: true, prestataireId: null });
    this.dialogVisible = true;
  }

  openEdit(service: ServiceDto): void {
    this.editingService = service;
    this.form.reset({
      nom: service.nom,
      description: service.description,
      isActive: service.isActive !== false,
      prestataireId: service.prestataireId ?? null
    });
    this.dialogVisible = true;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new CreateServiceCommand({
      nom: value.nom,
      description: value.description,
      isActive: value.isActive,
      prestataireId: value.prestataireId ?? undefined
    });

    if (this.editingService) {
      const updateCommand = new UpdateServiceCommand({
        id: this.editingService.id,
        nom: value.nom,
        description: value.description,
        isActive: value.isActive,
        prestataireId: value.prestataireId ?? undefined
      });

      this.servicesClient.updateService(this.editingService.id, updateCommand).subscribe({
        next: () => this.refreshAfterSave(),
        error: () => this.errorMessage = 'Impossible de mettre à jour le service.'
      });
      return;
    }

    this.servicesClient.createService(command).subscribe({
      next: () => this.refreshAfterSave(),
      error: () => this.errorMessage = 'Impossible de créer le service.'
    });
  }

  toggleActive(service: ServiceDto): void {
    const command = new UpdateServiceCommand({
      id: service.id,
      nom: service.nom,
      description: service.description,
      isActive: !(service.isActive ?? true),
      prestataireId: service.prestataireId ?? undefined
    });

    this.servicesClient.updateService(service.id, command).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage = 'Impossible de changer le statut du service.'
    });
  }

  delete(service: ServiceDto): void {
    if (!service.id) {
      return;
    }

    this.servicesClient.deleteService(service.id).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage = 'Impossible de supprimer le service.'
    });
  }

  private refreshAfterSave(): void {
    this.dialogVisible = false;
    this.load();
  }
}
