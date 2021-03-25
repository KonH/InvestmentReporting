import { createStore } from 'vuex';
import AppState from '@/store/appState';
import { StateDto } from '@/api/state';
import Backend from '@/service/backend';

export default createStore({
	state() {
		return new AppState();
	},
	mutations: {
		applyActiveState(appState: AppState, activeState: StateDto) {
			appState.activeState = activeState;
		},
	},
	actions: {
		async fetchActiveState({ commit }) {
			const date = new Date().toISOString();
			const response = await Backend.state().state.stateList({ date: date });
			commit('applyActiveState', response.data);
		},
	},
	modules: {},
});
