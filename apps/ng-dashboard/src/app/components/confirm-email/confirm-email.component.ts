import {Component, ElementRef, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Store} from '@ngrx/store';
import {Observable} from 'rxjs/Observable';
import {selectConfirmEmailError, selectConfirmEmailSuccess} from '@app/states/auth/auth.reducers';
import {confirmEmail} from '@app/states/auth/auth.actions';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class ConfirmEmailComponent implements OnInit {
  @ViewChild('card') cardElm: ElementRef;

  isSuccess: boolean;
  inProgress = false;
  private confirmEmailError$: Observable<any>;
  private confirmEmailSuccess$: Observable<any>;

  constructor(private route: ActivatedRoute, private store: Store) {
    this.confirmEmailError$ = this.store.select(selectConfirmEmailError);
    this.confirmEmailSuccess$ = this.store.select(selectConfirmEmailSuccess);
  }

  ngOnInit(): void {
    this.confirmEmailError$.subscribe((error) => {
      console.log('confirm email error', error);
      this.inProgress = false;
      this.isSuccess = false;
    });
    this.confirmEmailSuccess$.subscribe((result) => {
      console.log('confirm email result', result);
      this.inProgress = false;
      this.isSuccess = true;
    });
    setTimeout(() => {
      // after 500 ms we add the class animated to the login/register card
      this.cardElm.nativeElement.classList.remove('card-hidden');
    }, 500);

    this.route.queryParams
      .subscribe(params => {
          const code = params.code;
          const userId = params.userId;
          this.inProgress = true;
          this.store.dispatch(confirmEmail({code, userId}));
        }
      );
  }

}
