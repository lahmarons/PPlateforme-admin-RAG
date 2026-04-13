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
                label: 'Quick Access',
                items: [
                    { label: 'API', icon: 'pi pi-fw pi-link', url: '/api', target: '_blank' },
                    { label: 'Account', icon: 'pi pi-fw pi-user', url: '/Identity/Account/Manage' }
                ]
            }
        ];
    }
}
