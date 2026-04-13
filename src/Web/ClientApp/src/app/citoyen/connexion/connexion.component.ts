import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { CitoyensClient } from '../../web-api-client';
import { CitoyenSessionService } from '../services/citoyen-session.service';

@Component({
  selector: 'app-citoyen-connexion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, CardModule, InputTextModule, PasswordModule, ButtonModule, MessageModule],
  templateUrl: './connexion.component.html'
})
export class ConnexionComponent {
  submitting = false;
  errorMessage: string | null = null;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    motDePasse: ['', [Validators.required]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly citoyensClient: CitoyensClient,
    private readonly session: CitoyenSessionService,
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
    this.citoyensClient.getCitoyens().subscribe({
      next: (citoyens) => {
        const citoyen = citoyens.find(c => c.email?.toLowerCase() === value.email.toLowerCase());

        if (!citoyen) {
          this.errorMessage = 'Aucun citoyen trouvé avec cet email.';
          this.submitting = false;
          return;
        }

        this.session.setConnected(citoyen);
        this.submitting = false;
        this.router.navigate(['/citoyen/dashboard']);
      },
      error: () => {
        this.errorMessage = 'Échec de connexion. Réessayez plus tard.';
        this.submitting = false;
      }
    });
  }
}
