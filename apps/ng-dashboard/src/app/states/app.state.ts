import {AuthState} from '@app/states/auth/auth.state';

// TODO lazy load assets states by separating routing config for Dashboard and Assets
export interface AppState {
  auth: AuthState;
}

