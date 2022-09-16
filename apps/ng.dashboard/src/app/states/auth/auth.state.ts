import {createSelector, createFeatureSelector} from '@ngrx/store';
import {UserDto} from '@app/models/dtos';

export const authKey = 'auth';
export interface AuthState {
  user: UserDto;
  token: string;
  loginError: any;
  registerError: any;
  confirmEmailSuccess: boolean;
  confirmEmailError: any;
}
