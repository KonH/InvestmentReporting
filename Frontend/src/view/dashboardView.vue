<template>
	<div>
		<h3>View</h3>
		TODO
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { DashboardConfigDto, DashboardStateDto } from '@/api/meta';
import { Prop } from 'vue-property-decorator';
import { Action, State } from 'vuex-class';

@Options({
	name: 'DashboardView',
})
export default class DashboardView extends Vue {
	@Action('fetchDashboardState')
	fetchDashboardState!: (id: string) => void;

	@State('dashboardStates')
	dashboardStates!: Map<string, DashboardStateDto>;

	@Prop()
	dashboardConfig!: DashboardConfigDto;

	get dashboardState() {
		return this.dashboardStates.get(this.dashboardConfig.id ?? '');
	}

	created() {
		this.tryFetch();
	}

	updated() {
		this.tryFetch();
	}

	tryFetch() {
		if (!this.dashboardState) {
			return;
		}
		this.fetchDashboardState(this.dashboardConfig.id ?? '');
	}
}
</script>
