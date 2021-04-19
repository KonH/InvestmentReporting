import { createStore } from 'vuex';
import AppState from '@/store/appState';
import { StateDto } from '@/api/state';
import Backend from '@/service/backend';
import { VirtualStateDto } from '@/api/market';
import { AssetTagStateDto, DashboardConfigStateDto, DashboardStateDto } from '@/api/meta';

export default createStore({
	state() {
		return new AppState();
	},
	mutations: {
		applyActiveState(appState: AppState, activeState: StateDto) {
			appState.activeState = activeState;
		},
		applySelectedVirtualStatePeriod(appState: AppState, period: string) {
			appState.selectedVirtualStatePeriod = period;
		},
		applySelectedVirtualStateBroker(appState: AppState, broker: string) {
			appState.selectedVirtualStateBroker = broker;
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
		applySelectedDashboardState(appState: AppState, state: DashboardStateDto) {
			appState.selectedDashboardState = state;
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
		async changeSelectedVirtualStatePeriod({ commit, dispatch }, period: string) {
			commit('applySelectedVirtualStatePeriod', period);
			dispatch('fetchVirtualState');
		},
		async changeSelectedVirtualStateBroker({ commit, dispatch }, broker: string) {
			commit('applySelectedVirtualStateBroker', broker);
			dispatch('fetchVirtualState');
		},
		async fetchVirtualState({ state, commit }) {
			const date = new Date().toISOString();
			const response = await Backend.market().virtualState.virtualStateList({
				date: date,
				period: state.selectedVirtualStatePeriod,
				broker: state.selectedVirtualStateBroker,
			});
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
		async fetchDashboardState({ commit }, dashboardId) {
			const date = new Date().toISOString();
			const response = await Backend.meta().dashboardState.dashboardStateList({
				date: date,
				dashboard: dashboardId,
			});
			commit('applySelectedDashboardState', response.data);
		},
	},
	modules: {},
});
