import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { PrestataireDto, PrestatairesClient } from '../../web-api-client';

@Injectable({
  providedIn: 'root'
})
export class PrestataireApiService {
  constructor(private readonly client: PrestatairesClient) {}

  getPrestataires(recherche?: string | null): Observable<PrestataireDto[]> {
    const term = recherche ?? undefined;

    return this.client.getPrestataires(undefined, term).pipe(
      catchError(() => throwError(() => new Error('Impossible de charger les prestataires.')))
    );
  }
}
