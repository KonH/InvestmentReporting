import { createStore } from 'vuex';
import AppState from '@/store/appState';
import { StateDto } from '@/api/state';
import Backend from '@/service/backend';
import { VirtualStateDto } from '@/api/market';

export default createStore({
	state() {
		return new AppState();
	},
	mutations: {
		applyActiveState(appState: AppState, activeState: StateDto) {
			appState.activeState = activeState;
		},
		applyVirtualState(appState: AppState, virtualState: VirtualStateDto) {
			appState.virtualState = virtualState;
		},
	},
	actions: {
		async fetchState({ dispatch }) {
			dispatch('fetchActiveState');
			dispatch('fetchVirtualState');
		},
		async fetchActiveState({ commit }) {
			const date = new Date().toISOString();
			const response = await Backend.state().state.stateList({ date: date });
			commit('applyActiveState', response.data);
		},
		async fetchVirtualState({ commit }) {
			const date = new Date().toISOString();
			const response = await Backend.market().virtualState.virtualStateList({ date: date });
			commit('applyVirtualState', response.data);
		},
	},
	modules: {},
});
