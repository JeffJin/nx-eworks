import {UserService} from '@app/services/user.service';
import {AbstractControl, AsyncValidatorFn, ValidationErrors, ValidatorFn} from '@angular/forms';
import 'rxjs/add/operator/debounceTime';
import {debounceTime, distinctUntilChanged, first, map, switchMap} from 'rxjs/operators';
import {Observable} from 'rxjs/Observable';

export class CustomValidators {
  static createEmailValidator(userService: UserService): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors> => {
      return control.valueChanges.pipe(
        debounceTime(500),
        distinctUntilChanged(),
        switchMap(email =>  userService.validateEmail(email)),
      ).pipe(first());
    };
  }

  static createUserNameValidator(userService: UserService): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors> => {
      return control.valueChanges.pipe(
          debounceTime(500),
          distinctUntilChanged(),
          switchMap(userName =>  userService.validateUserName(userName)),
        ).pipe(first());
    };
  }

  static validatePasswordsMatch(targetControl: AbstractControl): ValidatorFn {
    return (control: AbstractControl): ValidationErrors => {
      return control.value === targetControl.value ? null : {
        passwordsNotMatch: true
      };
    };
  }
}

