import { Component } from '@angular/core';

@Component({
    selector: 'app-auth-footer',
    templateUrl: 'auth-footer.component.html'
})

export class AuthFooterComponent {
    today: Date = new Date();
}
