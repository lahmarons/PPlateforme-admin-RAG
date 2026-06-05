import { Injectable, computed, signal } from '@angular/core';

export interface CitizenSessionState {
  id: number | null;
  userName: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class CitoyenSessionService {
  private readonly _connected = signal(false);
  private readonly _citoyen = signal<CitizenSessionState | null>(null);

  readonly isConnected = computed(() => this._connected());
  readonly citoyen = this._citoyen.asReadonly();

  markConnected(userName: string, id: number | null = null): void {
    this._connected.set(true);
    this._citoyen.set({ id, userName });
  }

  clear(): void {
    this._connected.set(false);
    this._citoyen.set(null);
  }
}
