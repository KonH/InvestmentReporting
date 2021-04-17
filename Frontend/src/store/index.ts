import { createStore } from 'vuex';
import AppState from '@/store/appState';
import { StateDto } from '@/api/state';
import Backend from '@/service/backend';
import { VirtualStateDto } from '@/api/market';
import { AssetTagStateDto, DashboardConfigStateDto } from '@/api/meta';

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
		applyTagState(appState: AppState, tagState: AssetTagStateDto) {
			appState.tagState = tagState;
		},
		applyDashboardConfigState(appState: AppState, dashboardConfigState: DashboardConfigStateDto) {
			appState.dashboardConfigState = dashboardConfigState;
		},
	},
	actions: {
		async fetchState({ dispatch }) {
			dispatch('fetchActiveState');
			dispatch('fetchVirtualState');
			dispatch('fetchTagState');
			dispatch('fetchDashboardConfigState');
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
		async fetchTagState({ commit }) {
			const response = await Backend.meta().tag.getTag();
			commit('applyTagState', response.data);
		},
		async fetchDashboardConfigState({ commit }) {
			const response = await Backend.meta().dashboardConfig.dashboardConfigList();
			commit('applyDashboardConfigState', response.data);
		},
	},
	modules: {},
});
