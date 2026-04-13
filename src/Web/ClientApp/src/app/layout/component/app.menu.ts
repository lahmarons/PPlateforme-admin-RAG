import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `<ul class="layout-menu">
        @for (item of model; track item.label) {
            @if (!item.separator) {
                <li app-menuitem [item]="item" [root]="true"></li>
            } @else {
                <li class="menu-separator"></li>
            }
        }
    </ul> `,
})
export class AppMenu {
    model: MenuItem[] = [];

    ngOnInit() {
        this.model = [
            {
                label: 'Navigation',
                items: [
                    { label: 'Home', icon: 'pi pi-fw pi-home', routerLink: ['/'] },
                    { label: 'Counter', icon: 'pi pi-fw pi-calculator', routerLink: ['/counter'] },
                    { label: 'Weather', icon: 'pi pi-fw pi-cloud', routerLink: ['/fetch-data'] }
                ]
            },
            {
                label: 'Espace citoyen',
                items: [
                    { label: 'Inscription', icon: 'pi pi-fw pi-user-plus', routerLink: ['/citoyen/inscription'] },
                    { label: 'Connexion', icon: 'pi pi-fw pi-sign-in', routerLink: ['/citoyen/connexion'] },
                    { label: 'Tableau de bord', icon: 'pi pi-fw pi-th-large', routerLink: ['/citoyen/dashboard'] },
                    { label: 'Services', icon: 'pi pi-fw pi-briefcase', routerLink: ['/citoyen/services'] },
                    { label: 'Mes demandes', icon: 'pi pi-fw pi-inbox', routerLink: ['/citoyen/demandes'] },
                    { label: 'Créer une demande', icon: 'pi pi-fw pi-send', routerLink: ['/citoyen/demandes/nouvelle'] },
                    { label: 'Chatbot', icon: 'pi pi-fw pi-comments', routerLink: ['/citoyen/chatbot'] }
                ]
            },
            {
                label: 'Espace agent',
                items: [
                    { label: 'Authentification', icon: 'pi pi-fw pi-sign-in', routerLink: ['/agent/connexion'] },
                    { label: 'Dashboard', icon: 'pi pi-fw pi-chart-line', routerLink: ['/agent/dashboard'] },
                    { label: 'Consulter demandes', icon: 'pi pi-fw pi-inbox', routerLink: ['/agent/demandes'] },
                    { label: 'Chatbot agent', icon: 'pi pi-fw pi-comments', routerLink: ['/agent/chatbot'] }
                ]
            },
            {
                label: 'Espace administrateur',
                items: [
                    { label: 'Connexion', icon: 'pi pi-fw pi-sign-in', routerLink: ['/admin/connexion'] },
                    { label: 'Dashboard', icon: 'pi pi-fw pi-chart-bar', routerLink: ['/admin/dashboard'] },
                    { label: 'Services', icon: 'pi pi-fw pi-briefcase', routerLink: ['/admin/services'] },
                    { label: 'Agents', icon: 'pi pi-fw pi-users', routerLink: ['/admin/agents'] }
                ]
            },
            {
                label: 'Quick Access',
                items: [
                    { label: 'API', icon: 'pi pi-fw pi-link', url: '/api', target: '_blank' },
                    { label: 'Account', icon: 'pi pi-fw pi-user', url: '/Identity/Account/Manage' }
                ]
            }
        ];
    }
}
