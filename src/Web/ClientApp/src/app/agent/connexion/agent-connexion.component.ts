import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { AppFloatingConfigurator } from '../../layout/component/app.floatingconfigurator';
import { AuthService } from '../../shared/auth/auth.service';

@Component({
  selector: 'app-agent-connexion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, InputTextModule, PasswordModule, ButtonModule, MessageModule, AppFloatingConfigurator],
  templateUrl: './agent-connexion.component.html'
})
export class AgentConnexionComponent {
  submitting = false;
  errorMessage: string | null = null;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    motDePasse: ['', [Validators.required]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  submit(event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();

    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = null;

    const value = this.form.getRawValue();
    this.authService.login(value.email, value.motDePasse).subscribe({
      next: (result) => {
        this.submitting = false;
        this.router.navigateByUrl(result.redirectUrl);
      },
      error: (error) => {
        this.errorMessage = this.authService.getErrorMessage(error, 'Identifiants invalides. Veuillez réessayer.');
        this.submitting = false;
      }
    });
  }
}
