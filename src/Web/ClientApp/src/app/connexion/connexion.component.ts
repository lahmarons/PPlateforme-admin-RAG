import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { AppFloatingConfigurator } from '../layout/component/app.floatingconfigurator';

@Component({
  selector: 'app-connexion-hub',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, AppFloatingConfigurator],
  templateUrl: './connexion.component.html'
})
export class ConnexionHubComponent {}
