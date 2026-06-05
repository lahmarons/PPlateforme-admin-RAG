import { Component, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';
import { AuthService } from '../../shared/auth/auth.service';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `<ul class="layout-menu">
        @for (item of model(); track item.label) {
            @if (!item.separator) {
                <li app-menuitem [item]="item" [root]="true"></li>
            } @else {
                <li class="menu-separator"></li>
            }
        }
    </ul> `,
})
export class AppMenu implements OnInit {
    private readonly authService = inject(AuthService);

    model = computed<MenuItem[]>(() => {
        const user = this.authService.currentUser();
        const roles = user?.roles ?? [];

        const menu: MenuItem[] = [];

        // Navigation section (always visible)
        menu.push({
            label: 'Navigation',
            items: [
                { label: 'Accueil', icon: 'pi pi-fw pi-home', routerLink: ['/'] }
            ]
        });

        if (!user) {
            // Unauthenticated: show login links for all spaces
            menu.push({
                label: 'Espace citoyen',
                items: [
                    { label: 'Inscription', icon: 'pi pi-fw pi-user-plus', routerLink: ['/citoyen/inscription'] },
                    { label: 'Connexion', icon: 'pi pi-fw pi-sign-in', routerLink: ['/citoyen/connexion'] }
                ]
            });
            menu.push({
                label: 'Espace agent',
                items: [
                    { label: 'Authentification', icon: 'pi pi-fw pi-sign-in', routerLink: ['/agent/connexion'] }
                ]
            });
            menu.push({
                label: 'Espace administrateur',
                items: [
                    { label: 'Connexion', icon: 'pi pi-fw pi-sign-in', routerLink: ['/admin/connexion'] }
                ]
            });
        } else if (roles.includes('Citoyen')) {
            menu.push({
                label: 'Espace citoyen',
                items: [
                    { label: 'Tableau de bord', icon: 'pi pi-fw pi-th-large', routerLink: ['/citoyen/dashboard'] },
                    { label: 'Services', icon: 'pi pi-fw pi-briefcase', routerLink: ['/citoyen/services'] },
                    { label: 'Prestataires', icon: 'pi pi-fw pi-building', routerLink: ['/prestataires'] },
                    { label: 'Mes demandes', icon: 'pi pi-fw pi-inbox', routerLink: ['/citoyen/demandes'] },
                    { label: 'Créer une demande', icon: 'pi pi-fw pi-send', routerLink: ['/citoyen/demandes/nouvelle'] },
                    { label: 'Chatbot', icon: 'pi pi-fw pi-comments', routerLink: ['/citoyen/chatbot'] }
                ]
            });
        } else if (roles.includes('Agent')) {
            menu.push({
                label: 'Espace agent',
                items: [
                    { label: 'Dashboard', icon: 'pi pi-fw pi-chart-line', routerLink: ['/agent/dashboard'] },
                    { label: 'Consulter demandes', icon: 'pi pi-fw pi-inbox', routerLink: ['/agent/demandes'] },
                    { label: 'Chatbot agent', icon: 'pi pi-fw pi-comments', routerLink: ['/agent/chatbot'] }
                ]
            });
        } else if (roles.includes('Administrator')) {
            menu.push({
                label: 'Espace administrateur',
                items: [
                    { label: 'Dashboard', icon: 'pi pi-fw pi-chart-bar', routerLink: ['/admin/dashboard'] },
                    { label: 'Services', icon: 'pi pi-fw pi-briefcase', routerLink: ['/admin/services'] },
                    { label: 'Agents', icon: 'pi pi-fw pi-users', routerLink: ['/admin/agents'] },
                    { label: 'Prestataires', icon: 'pi pi-fw pi-building', routerLink: ['/admin/prestataires'] }
                ]
            });
        }

        return menu;
    });

    ngOnInit() {
        // Fetch current auth state on app load to populate the sidebar
        this.authService.me().subscribe();
    }
}
