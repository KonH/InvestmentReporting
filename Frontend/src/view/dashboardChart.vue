<template>
	<canvas id="chart" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import Chart from 'chart.js';
import { DashboardStateDto, DashboardStateTagDto } from '@/api/meta';

@Options({
	name: 'DashboardChart',
})
export default class DashboardChart extends Vue {
	chartCanvas!: HTMLCanvasElement | undefined;

	@Prop()
	dashboard!: DashboardStateDto;

	@Prop()
	currencyCode!: string;

	chart: Chart | undefined;

	mounted() {
		this.onUpdate();
		this.$watch('dashboard', this.onUpdate);
		this.$watch('chartCanvas', this.onUpdate);
	}

	updated() {
		this.onUpdate();
	}

	onUpdate() {
		if (!this.chartCanvas) {
			this.chartCanvas = document.getElementById('chart') as HTMLCanvasElement;
		}
		if (this.chart) {
			this.chart.config = this.getConfig();
			this.chart.update();
		} else {
			if (this.chartCanvas) {
				this.chart = new Chart(this.chartCanvas, this.getConfig());
			}
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
		const totalSums = this.dashboard.sums;
		const totalSumHolder = totalSums ? totalSums[this.currencyCode] : undefined;
		const totalSum = totalSumHolder ? totalSumHolder.virtualSum ?? 1 : 1;
		return tags.map((t) => {
			const sums = t.sums;
			const sumHolder = sums ? sums[this.currencyCode] : undefined;
			const sum = sumHolder ? sumHolder.virtualSum ?? 0 : 0;
			const percent = (sum / totalSum) * 100;
			const pow = Math.pow(10, 2);
			return Math.round(percent * pow) / pow;
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
