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
	chart!: HTMLCanvasElement;

	@Prop()
	dashboard!: DashboardStateDto;

	@Prop()
	currencyId!: string;

	mounted() {
		const tags = this.dashboard.tags ?? [];
		const options = {
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
		new Chart(this.chart, options);
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
