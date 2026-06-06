import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AppFloatingConfigurator } from '../../layout/component/app.floatingconfigurator';

@Component({
  selector: 'app-auth-error',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, AppFloatingConfigurator],
  templateUrl: './auth-error.component.html'
})
export class AuthErrorComponent {}
