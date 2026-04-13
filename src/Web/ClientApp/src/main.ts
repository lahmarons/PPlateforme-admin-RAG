import { enableProdMode, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async'; // New
import { providePrimeNG } from 'primeng/config'; // New
import Aura from '@primeuix/themes/aura'; // New
import { ModalModule } from 'ngx-bootstrap/modal';

import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { AuthorizeInterceptor } from './api-authorization/authorize.interceptor';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
  provideRouter(routes),
  provideHttpClient(withInterceptorsFromDi()),
  provideAnimationsAsync(), // Replaces BrowserAnimationsModule for better performance
  providePrimeNG({ 
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: '.app-dark'
      }
    }
  }),
  importProvidersFrom(ModalModule)
];

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    ...providers
  ]
}).catch(err => console.log(err));