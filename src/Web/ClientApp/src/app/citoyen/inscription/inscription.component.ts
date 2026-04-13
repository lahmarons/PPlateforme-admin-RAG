import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { CitoyensClient, CreateCitoyenCommand } from '../../web-api-client';

@Component({
  selector: 'app-citoyen-inscription',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, CardModule, InputTextModule, PasswordModule, ButtonModule, MessageModule],
  templateUrl: './inscription.component.html'
})
export class InscriptionComponent {
  submitting = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  readonly form = this.fb.nonNullable.group({
    nom: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    motDePasse: ['', [Validators.required, Validators.minLength(6)]],
    adresse: ['', [Validators.required]],
    telephone: ['', [Validators.required]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly citoyensClient: CitoyensClient,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = null;
    this.successMessage = null;

    const value = this.form.getRawValue();
    const command = new CreateCitoyenCommand({
      nom: value.nom,
      email: value.email,
      motDePasse: value.motDePasse,
      adresse: value.adresse,
      telephone: value.telephone
    });

    this.citoyensClient.createCitoyen(command).subscribe({
      next: () => {
        this.successMessage = 'Inscription réussie. Vous pouvez vous connecter.';
        this.form.reset();
        this.submitting = false;
        setTimeout(() => this.router.navigate(['/citoyen/connexion']), 600);
      },
      error: () => {
        this.errorMessage = 'Échec de l’inscription. Vérifiez les données et réessayez.';
        this.submitting = false;
      }
    });
  }
}
