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
import { AgentDto, AgentsClient, CreateAgentCommand, ServiceDto, ServicesClient, UpdateAgentCommand } from '../../web-api-client';

@Component({
  selector: 'app-admin-agents',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, ButtonModule, CardModule, DialogModule, InputTextModule, MessageModule, SelectModule, TableModule, TagModule],
  templateUrl: './admin-agents.component.html'
})
export class AdminAgentsComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  agents: AgentDto[] = [];
  services: ServiceDto[] = [];
  dialogVisible = false;
  editingAgent: AgentDto | null = null;
  readonly statusOptions = [
    { label: 'Actif', value: true },
    { label: 'Désactivé', value: false }
  ];

  readonly form = this.fb.nonNullable.group({
    nom: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    motDePasse: ['', [Validators.required]],
    matricule: ['', [Validators.required]],
    grade: ['', [Validators.required]],
    isActive: [true],
    serviceId: [null as number | null]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly agentsClient: AgentsClient,
    private readonly servicesClient: ServicesClient
  ) {}

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    try {
      const [agents, services] = await Promise.all([
        firstValueFrom(this.agentsClient.getAgents()),
        firstValueFrom(this.servicesClient.getServices())
      ]);

      this.agents = agents;
      this.services = services;
    } catch {
      this.errorMessage = 'Impossible de charger les agents.';
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    this.editingAgent = null;
    this.form.reset({ nom: '', email: '', motDePasse: '', matricule: '', grade: '', isActive: true, serviceId: null });
    this.dialogVisible = true;
  }

  openEdit(agent: AgentDto): void {
    this.editingAgent = agent;
    this.form.reset({
      nom: agent.nom,
      email: agent.email,
      motDePasse: '',
      matricule: agent.matricule,
      grade: agent.grade,
      isActive: agent.isActive !== false,
      serviceId: agent.serviceId || null
    });
    this.dialogVisible = true;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      nom: value.nom,
      email: value.email,
      motDePasse: value.motDePasse,
      matricule: value.matricule,
      grade: value.grade,
      isActive: value.isActive,
      serviceId: value.serviceId ?? 0
    };

    if (this.editingAgent) {
      const command = new UpdateAgentCommand({
        id: this.editingAgent.id,
        ...payload,
        serviceId: value.serviceId
      });

      this.agentsClient.updateAgent(this.editingAgent.id, command).subscribe({
        next: () => this.finishSave(),
        error: () => this.errorMessage = 'Impossible de mettre à jour l’agent.'
      });
      return;
    }

    const command = new CreateAgentCommand(payload);
    this.agentsClient.createAgent(command).subscribe({
      next: () => this.finishSave(),
      error: () => this.errorMessage = 'Impossible de créer l’agent.'
    });
  }

  toggleActive(agent: AgentDto): void {
    const command = new UpdateAgentCommand({
      id: agent.id,
      nom: agent.nom,
      email: agent.email,
      motDePasse: '',
      matricule: agent.matricule,
      grade: agent.grade,
      isActive: !(agent.isActive ?? true),
      serviceId: agent.serviceId || null
    });

    this.agentsClient.updateAgent(agent.id, command).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage = 'Impossible de changer le statut de l’agent.'
    });
  }

  private finishSave(): void {
    this.dialogVisible = false;
    this.load();
  }
}
