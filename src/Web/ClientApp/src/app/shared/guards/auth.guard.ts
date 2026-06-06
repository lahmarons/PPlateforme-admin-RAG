import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { catchError, map, of } from 'rxjs';

export const roleGuard = (requiredRole: string): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const checkRole = (user: any) => {
      if (!user) {
        router.navigate(['/connexion']);
        return false;
      }
      
      const roles = user.roles ?? [];
      if (roles.includes(requiredRole)) {
        return true;
      }

      router.navigate(['/auth/error']);
      return false;
    };

    const currentUser = authService.currentUser();
    if (currentUser) {
      return checkRole(currentUser);
    }

    return authService.me().pipe(
      map(user => checkRole(user)),
      catchError(() => {
        router.navigate(['/connexion']);
        return of(false);
      })
    );
  };
};

export const citoyenGuard = roleGuard('Citoyen');
export const agentGuard = roleGuard('Agent');
export const adminGuard = roleGuard('Administrator');
