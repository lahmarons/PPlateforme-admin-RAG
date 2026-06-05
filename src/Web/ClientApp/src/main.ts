import { enableProdMode, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async'; // New
import { providePrimeNG } from 'primeng/config'; // New
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura'; // New
import { ModalModule } from 'ngx-bootstrap/modal';

import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { AuthorizeInterceptor } from './api-authorization/authorize.interceptor';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const RosePreset = definePreset(Aura, {
  semantic: {
    primary: Aura.primitive.rose,
    colorScheme: {
      light: {
        primary: {
          color: '{primary.500}',
          contrastColor: '#ffffff',
          hoverColor: '{primary.600}',
          activeColor: '{primary.700}'
        },
        highlight: {
          background: '{primary.50}',
          focusBackground: '{primary.100}',
          color: '{primary.700}',
          focusColor: '{primary.800}'
        }
      },
      dark: {
        primary: {
          color: '{primary.400}',
          contrastColor: '{surface.900}',
          hoverColor: '{primary.300}',
          activeColor: '{primary.200}'
        },
        highlight: {
          background: 'color-mix(in srgb, {primary.400}, transparent 84%)',
          focusBackground: 'color-mix(in srgb, {primary.400}, transparent 76%)',
          color: 'rgba(255,255,255,.87)',
          focusColor: 'rgba(255,255,255,.87)'
        }
      }
    }
  }
});

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
  provideRouter(routes),
  provideHttpClient(withInterceptorsFromDi()),
  provideAnimationsAsync(), // Replaces BrowserAnimationsModule for better performance
  providePrimeNG({ 
    theme: {
      preset: RosePreset,
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