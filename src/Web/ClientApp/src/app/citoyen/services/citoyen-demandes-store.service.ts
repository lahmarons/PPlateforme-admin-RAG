import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CitoyenDemandesStoreService {
  private readonly _demandeIds = signal<number[]>([]);

  readonly demandeIds = this._demandeIds.asReadonly();

  addDemandeId(id: number): void {
    const current = this._demandeIds();
    if (current.includes(id)) {
      return;
    }

    this._demandeIds.set([id, ...current]);
  }

  clear(): void {
    this._demandeIds.set([]);
  }
}
