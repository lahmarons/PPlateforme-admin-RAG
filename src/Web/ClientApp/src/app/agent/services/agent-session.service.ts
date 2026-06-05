import { Injectable, computed, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AgentSessionService {
  private readonly _connected = signal(false);

  readonly isConnected = computed(() => this._connected());

  markConnected(): void {
    this._connected.set(true);
  }

  clear(): void {
    this._connected.set(false);
  }
}
