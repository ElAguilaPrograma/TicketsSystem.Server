import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import { Cyan, Emerald, MyPresets } from '../ThemePreset';
import { providePrimeNG } from 'primeng/config';
import { provideAnimations } from '@angular/platform-browser/animations';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    providePrimeNG({
      theme: {
        preset: MyPresets[localStorage.getItem("color-preset") || "Emerald"] || Emerald,
        options: {
          darkModeSelector: '.my-app-dark'
        }
      }
    }),
    provideAnimations()
  ]
};
