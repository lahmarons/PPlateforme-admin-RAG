import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { firstValueFrom } from 'rxjs';
import { CreatePrestataireCommand, PrestataireDto, PrestatairesClient, UpdatePrestataireCommand } from '../../web-api-client';

@Component({
  selector: 'app-admin-prestataires',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, ButtonModule, DialogModule, InputTextModule, MessageModule, TableModule],
  templateUrl: './admin-prestataires.component.html'
})
export class AdminPrestatairesComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  prestataires: PrestataireDto[] = [];
  dialogVisible = false;
  editing: PrestataireDto | null = null;
  logoBase64: string | null = null;

  readonly form = this.fb.nonNullable.group({
    nom: ['', [Validators.required]],
    description: ['', [Validators.required]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly prestatairesClient: PrestatairesClient
  ) {}

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    try {
      this.prestataires = await firstValueFrom(this.prestatairesClient.getPrestataires(undefined, undefined));    } catch {
      this.errorMessage = 'Impossible de charger les prestataires.';
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    this.editing = null;
    this.logoBase64 = null;
    this.form.reset({
      nom: '',
      description: ''
    });
    this.dialogVisible = true;
  }

  openEdit(prestataire: PrestataireDto): void {
    this.editing = prestataire;
    this.logoBase64 = prestataire.logo || null;
    this.form.reset({
      nom: prestataire.nom,
      description: prestataire.description
    });
    this.dialogVisible = true;
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (file.size > 1024 * 1024) {
      alert("L'image est trop volumineuse (max 1Mo)");
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      const result = reader.result as string;
      const base64Index = result.indexOf('base64,');
      if (base64Index !== -1) {
        this.logoBase64 = result.substring(base64Index + 7);
      } else {
        this.logoBase64 = result;
      }
    };
    input.value = '';
  }

  removeLogo(): void {
    this.logoBase64 = null;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (this.editing) {
      const command = new UpdatePrestataireCommand({
        id: this.editing.id,
        nom: value.nom,
        description: value.description,
        logo: this.logoBase64 || undefined
      });

      this.prestatairesClient.updatePrestataire(this.editing.id!, command).subscribe({
        next: () => this.finishSave(),
        error: () => this.errorMessage = 'Impossible de mettre à jour le prestataire.'
      });
      return;
    }

    const command = new CreatePrestataireCommand({
      nom: value.nom,
      description: value.description,
      logo: this.logoBase64 || undefined
    });

    this.prestatairesClient.createPrestataire(command).subscribe({
      next: () => this.finishSave(),
      error: () => this.errorMessage = 'Impossible de créer le prestataire.'
    });
  }

  delete(prestataire: PrestataireDto): void {
    if (!prestataire.id) {
      return;
    }

    this.prestatairesClient.deletePrestataire(prestataire.id).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage = 'Impossible de supprimer le prestataire.'
    });
  }

  private finishSave(): void {
    this.dialogVisible = false;
    this.load();
  }
}
