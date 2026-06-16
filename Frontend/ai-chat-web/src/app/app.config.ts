import { ApplicationConfig, provideBrowserGlobalErrorListeners ,importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    importProvidersFrom(FormsModule),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes)
  ]
};
