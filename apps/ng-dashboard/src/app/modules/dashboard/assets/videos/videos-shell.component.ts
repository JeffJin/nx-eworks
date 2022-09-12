import {ChangeDetectionStrategy, Component, OnDestroy, OnInit, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Subject} from 'rxjs/Subject';

@Component({
  selector: 'app-videos',
  templateUrl: './videos-shell.component.html',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VideosShellComponent implements OnInit {
  private errorSubject = new Subject<string>();
  errorMessage$ = this.errorSubject.asObservable();

  constructor(
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
  ) {
  }

  ngOnInit(): void {
  }

}
