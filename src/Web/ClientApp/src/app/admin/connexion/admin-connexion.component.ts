import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { AdminsClient } from '../../web-api-client';
import { AdminSessionService } from '../services/admin-session.service';

@Component({
  selector: 'app-admin-connexion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, InputTextModule, PasswordModule, ButtonModule, MessageModule],
  templateUrl: './admin-connexion.component.html'
})
export class AdminConnexionComponent {
  submitting = false;
  errorMessage: string | null = null;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    motDePasse: ['', [Validators.required]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly adminsClient: AdminsClient,
    private readonly session: AdminSessionService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = null;

    const value = this.form.getRawValue();
    this.adminsClient.getAdmins().subscribe({
      next: (admins) => {
        const admin = admins.find(a => a.email?.toLowerCase() === value.email.toLowerCase());

        if (!admin) {
          this.errorMessage = 'Aucun administrateur trouvé avec cet email.';
          this.submitting = false;
          return;
        }

        this.session.setConnected(admin);
        this.submitting = false;
        this.router.navigate(['/admin/dashboard']);
      },
      error: () => {
        this.errorMessage = 'Échec de connexion administrateur.';
        this.submitting = false;
      }
    });
  }
}
