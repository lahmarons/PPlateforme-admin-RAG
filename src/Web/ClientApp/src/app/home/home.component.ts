import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    ButtonModule,
    DividerModule,
    InputTextModule
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  searchQuery = '';

  scrollToRoleSelection(): void {
    const element = document.getElementById('role-selection');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  search(): void {
    if (this.searchQuery.trim()) {
      alert(`Recherche pour "${this.searchQuery}" : Les services de démonstration sont listés ci-dessous. Veuillez vous connecter pour soumettre et suivre des demandes.`);
    }
  }
}
