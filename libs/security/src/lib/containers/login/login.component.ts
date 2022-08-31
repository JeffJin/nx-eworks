import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'auth-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent implements OnInit {
  constructor() {}

  ngOnInit() {}

  login(authenticate: any) {
    console.log(authenticate);
  }
}
