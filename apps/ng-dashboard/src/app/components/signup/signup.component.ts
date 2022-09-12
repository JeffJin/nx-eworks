import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {AuthService} from '../../services/auth.service';
import {FormBuilder, UntypedFormControl, FormGroupDirective, NgForm, Validators} from '@angular/forms';
import {CommonErrorStateMatcher} from '../../models/commmon-error-state-matcher';
import {Router} from '@angular/router';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class SignupComponent implements OnInit {
  error: any = null;

  email = new UntypedFormControl('', [
    Validators.required,
    Validators.email,
  ]);
  password = new UntypedFormControl('', [
    Validators.required
  ]);
  confirmPassword = new UntypedFormControl('', [
    Validators.required
  ]);

  agreeTnC = new UntypedFormControl('', [
    Validators.required
  ]);

  matcher = new CommonErrorStateMatcher();

  constructor(private authService: AuthService, private router: Router) { }


  ngOnInit() {

  }

  register() {
    console.log(this.email.value, this.password.value, this.confirmPassword.value);
    this.authService.register(this.email.value, this.email.value,
      this.password.value, this.confirmPassword.value).subscribe((result) => {
        if(result.succeeded){
          this.router.navigateByUrl('login');
        }
        else{
          this.error = result;
        }
    });
  }

  facebookSignup() {

  }

  googleSignup() {

  }
}
