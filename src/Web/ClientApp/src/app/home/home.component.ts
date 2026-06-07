import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { AuthService } from '../shared/auth/auth.service';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

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
export class HomeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  searchQuery = '';

  ngOnInit(): void {
    this.authService.me().pipe(
      catchError(() => of(null))
    ).subscribe();
  }

  isLoggedIn(): boolean {
    return this.authService.currentUser() !== null;
  }

  goToMySpace(): void {
    const user = this.authService.currentUser();
    if (user) {
      const roles = user.roles ?? [];
      if (roles.includes('Citoyen')) {
        this.router.navigate(['/citoyen/dashboard']);
      } else if (roles.includes('Agent')) {
        this.router.navigate(['/agent/dashboard']);
      } else if (roles.includes('Administrator')) {
        this.router.navigate(['/admin/dashboard']);
      } else {
        this.router.navigate(['/connexion']);
      }
    } else {
      this.scrollToRoleSelection();
    }
  }

  scrollToRoleSelection(): void {
    const element = document.getElementById('role-selection');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  search(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/citoyen/services'], { queryParams: { q: this.searchQuery.trim() } });
    }
  }
}
