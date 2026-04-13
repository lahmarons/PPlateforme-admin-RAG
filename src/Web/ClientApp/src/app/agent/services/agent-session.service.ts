import { Injectable, computed, signal } from '@angular/core';
import { AgentDto } from '../../web-api-client';

@Injectable({
  providedIn: 'root'
})
export class AgentSessionService {
  private readonly _agent = signal<AgentDto | null>(null);

  readonly agent = this._agent.asReadonly();
  readonly isConnected = computed(() => this._agent() !== null);

  setConnected(agent: AgentDto): void {
    this._agent.set(agent);
  }

  clear(): void {
    this._agent.set(null);
  }
}
