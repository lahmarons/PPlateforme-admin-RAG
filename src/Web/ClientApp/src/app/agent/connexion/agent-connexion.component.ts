import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { AgentsClient } from '../../web-api-client';
import { AgentSessionService } from '../services/agent-session.service';

@Component({
  selector: 'app-agent-connexion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, InputTextModule, PasswordModule, ButtonModule, MessageModule],
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
    private readonly agentsClient: AgentsClient,
    private readonly session: AgentSessionService,
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
    this.agentsClient.getAgents().subscribe({
      next: (agents) => {
        const agent = agents.find(a => a.email?.toLowerCase() === value.email.toLowerCase());

        if (!agent) {
          this.errorMessage = 'Identifiants invalides. Veuillez réessayer.';
          this.submitting = false;
          return;
        }

        this.session.setConnected(agent);
        this.submitting = false;
        this.router.navigate(['/agent/dashboard']);
      },
      error: () => {
        this.errorMessage = 'Erreur de connexion agent. Réessayez plus tard.';
        this.submitting = false;
      }
    });
  }
}
