import { Injectable, computed, signal } from '@angular/core';
import { CitoyenDto } from '../../web-api-client';

@Injectable({
  providedIn: 'root'
})
export class CitoyenSessionService {
  private readonly _citoyen = signal<CitoyenDto | null>(null);

  readonly citoyen = this._citoyen.asReadonly();
  readonly isConnected = computed(() => this._citoyen() !== null);

  setConnected(citoyen: CitoyenDto): void {
    this._citoyen.set(citoyen);
  }

  clear(): void {
    this._citoyen.set(null);
  }
}
