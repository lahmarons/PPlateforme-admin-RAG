import { Injectable, computed, signal } from '@angular/core';
import { AdminDto } from '../../web-api-client';

@Injectable({
  providedIn: 'root'
})
export class AdminSessionService {
  private readonly _admin = signal<AdminDto | null>(null);

  readonly admin = this._admin.asReadonly();
  readonly isConnected = computed(() => this._admin() !== null);

  setConnected(admin: AdminDto): void {
    this._admin.set(admin);
  }

  clear(): void {
    this._admin.set(null);
  }
}
