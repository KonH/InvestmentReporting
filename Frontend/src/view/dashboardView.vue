<template>
	<div>
		<h3>View</h3>
		<label>
			Currency:
			<select ref="currency" v-model="selectedCurrency" class="form-control">
				<option v-for="currency in currencies" :key="currency.id" :value="currency.id">
					{{ currency.code }}
				</option>
			</select>
		</label>
		<div v-if="selectedCurrency">
			<dashboard-legend :dashboard="selectedDashboardState" :dashboard-config="dashboardConfig" :currency-id="selectedCurrency" />
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { DashboardConfigDto, DashboardStateDto } from '@/api/meta';
import { Prop } from 'vue-property-decorator';
import { Action, State } from 'vuex-class';
import DashboardLegend from '@/view/dashboardLegend.vue';
import { StateDto } from '@/api/state';

@Options({
	name: 'DashboardView',
	components: {
		DashboardLegend,
	},
})
export default class DashboardView extends Vue {
	@Action('fetchDashboardState')
	fetchDashboardState!: (id: string) => void;

	@State('selectedDashboardState')
	selectedDashboardState!: DashboardStateDto;

	@State('activeState')
	activeState!: StateDto;

	@Prop()
	dashboardConfig!: DashboardConfigDto;

	selectedCurrency = '';

	get currencies() {
		return this.activeState.currencies;
	}
}
</script>
