import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-footer',
    template: `<div class="layout-footer">
        IdaraTech by
        <a href="https://gitlab.com/moezzmanter" target="_blank" rel="noopener noreferrer" class="text-primary font-bold hover:underline">Moez & Ons</a>
    </div>`
})
export class AppFooter {}
