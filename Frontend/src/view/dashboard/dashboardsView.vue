<template>
	<div class="row mt-3 pl-3">
		<div v-for="dashboard in dashboards" :key="dashboard.id">
			<dashboard-card :dashboard="dashboard" :active="isSelected(dashboard.id)" class="mr-2" @click="onDashboardClick(dashboard.id)" />
		</div>
		<add-dashboard-card @click="onNewDashboardClick()" />
	</div>
	<dashboard-config v-if="selectedDashboard" :dashboard="selectedDashboard" @save="onConfigSave" @remove="onConfigRemove" />
	<dashboard-view v-if="selectedDashboard && selectedDashboardState" :dashboard-config="selectedDashboard" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { DashboardConfigDto, DashboardConfigStateDto, DashboardStateDto } from '@/api/meta';
import DashboardCard from '@/component/dashboard/dashboardCard.vue';
import AddDashboardCard from '@/component/dashboard/addDashboardCard.vue';
import DashboardConfig from '@/component/dashboard/dashboardConfig.vue';
import DashboardView from '@/view/dashboard/dashboardView.vue';
import { Action, State } from 'vuex-class';

@Options({
	name: 'DashboardsView',
	components: {
		DashboardCard,
		AddDashboardCard,
		DashboardConfig,
		DashboardView,
	},
})
export default class DashboardsView extends Vue {
	@State('dashboardConfigState')
	dashboardConfigState!: DashboardConfigStateDto;

	@State('selectedDashboardState')
	selectedDashboardState!: DashboardStateDto | undefined;

	@Action('fetchDashboardConfigState')
	fetchDashboardConfigState!: () => void;

	@Action('fetchDashboardState')
	fetchDashboardState!: (dashboardId: string) => void;

	selectedDashboardId = '';
	hasNewDashboard = false;

	get stateDashboards() {
		return this.dashboardConfigState.dashboards ?? [];
	}

	newDashboard(): DashboardConfigDto {
		return {
			id: '',
			name: 'New Dashboard',
			tags: [],
		};
	}

	get dashboards() {
		return this.hasNewDashboard ? this.stateDashboards.concat([this.newDashboard()]) : this.stateDashboards;
	}

	get selectedDashboard() {
		return this.dashboards.find((d) => d.id == this.selectedDashboardId);
	}

	isSelected(dashboardId: string) {
		return dashboardId == this.selectedDashboardId;
	}

	onDashboardClick(dashboardId: string) {
		this.selectedDashboardId = dashboardId;
		if (dashboardId) {
			this.fetchDashboardState(this.selectedDashboardId);
		}
	}

	onNewDashboardClick() {
		this.hasNewDashboard = true;
		this.selectedDashboardId = this.newDashboard().id ?? '';
	}

	onConfigSave() {
		this.hasNewDashboard = false;
		this.fetchDashboardConfigState();
		this.fetchDashboardState(this.selectedDashboardId);
	}

	onConfigRemove() {
		this.hasNewDashboard = false;
		this.selectedDashboardId = '';
	}
}
</script>
