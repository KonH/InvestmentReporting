<template>
	<canvas ref="chart" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop, Ref } from 'vue-property-decorator';
import Chart from 'chart.js';
import { DashboardStateDto, DashboardStateTagDto } from '@/api/meta';

@Options({
	name: 'DashboardChart',
})
export default class DashboardChart extends Vue {
	@Ref('chart')
	canvas!: HTMLCanvasElement;

	@Prop()
	dashboard!: DashboardStateDto;

	@Prop()
	currencyId!: string;

	chart: Chart | undefined;

	mounted() {
		this.chart = new Chart(this.canvas, this.getConfig());
	}

	updated() {
		if (this.chart) {
			this.chart.config = this.getConfig();
		}
	}

	getConfig() {
		const tags = this.dashboard.tags ?? [];
		return {
			type: 'doughnut',
			data: {
				datasets: [
					{
						data: this.getData(tags),
						backgroundColor: this.getColors(tags.length),
					},
				],
				labels: this.getLabels(tags),
			},
		};
	}

	getData(tags: DashboardStateTagDto[]) {
		return tags.map((t) => {
			const sums = t.sums;
			return sums ? sums[this.currencyId].virtualSum : 0;
		});
	}

	getColors(count: number) {
		return ['red', 'green', 'blue', 'yellow', 'orange', 'lightblue', 'purple', 'brown', 'magenta', 'gray'].slice(0, count);
	}

	getLabels(tags: DashboardStateTagDto[]) {
		return tags.map((t) => t.tag ?? '');
	}
}
</script>
