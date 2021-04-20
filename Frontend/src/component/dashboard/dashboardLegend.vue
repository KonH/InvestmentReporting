<template>
	<div>
		<b>Total:</b> <money :value="sum" :currency-code="currencyCode" />
		<ul>
			<li v-for="tag in dashboard.tags" :key="tag.tag">
				<b>{{ tag.tag }}</b
				>: <money :value="getTagSum(tag.tag)" :currency-code="currencyCode" /> <b>{{ getTagPercentFormat(tag.tag) }}% </b>
				<span :style="getTagPercentStyle(tag.tag)"> {{ getTagPercentTargetDiff(tag.tag) }}%</span>
				<ul>
					<li v-for="asset in tag.assets" :key="asset.isin">
						<b>{{ asset.isin }}</b> {{ asset.name }} <money :value="getAssetSum(asset.sums)" :currency-code="currencyCode" />
					</li>
				</ul>
			</li>
		</ul>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { DashboardConfigDto, DashboardStateDto, SumStateDto } from '@/api/meta';
import Money from '@/component/common/money.vue';

@Options({
	name: 'DashboardLegend',
	components: {
		Money,
	},
})
export default class DashboardLegend extends Vue {
	@Prop()
	dashboardConfig!: DashboardConfigDto;

	@Prop()
	dashboard!: DashboardStateDto;

	@Prop()
	currencyCode!: string;

	get sum() {
		const sums = this.dashboard.sums;
		const sum = sums ? sums[this.currencyCode] : undefined;
		return sum ? sum.virtualSum : 0;
	}

	getTagSum(tag: string) {
		const tags = this.dashboard.tags;
		if (!tags) {
			return 0;
		}
		const sums = tags.find((t) => t.tag == tag)?.sums;
		const sum = sums ? sums[this.currencyCode] : undefined;
		return sum ? sum.virtualSum : 0;
	}

	getTagPercentValue(tag: string) {
		const tagSum = this.getTagSum(tag) ?? 0;
		const sum = this.sum ?? 0;
		return (tagSum / sum) * 100;
	}

	getTagPercentFormat(tag: string) {
		return this.getTagPercentValue(tag).toFixed(2);
	}

	getTagTarget(tag: string) {
		const tagConfig = this.dashboardConfig.tags?.find((t) => t.tag == tag);
		return tagConfig?.target ?? 0;
	}

	getTagPercentTargetDiffValue(tag: string) {
		const percent = this.getTagPercentValue(tag);
		const target = this.getTagTarget(tag);
		return percent - target;
	}

	getTagPercentTargetDiff(tag: string) {
		const value = this.getTagPercentTargetDiffValue(tag);
		const str = value.toFixed(2);
		return value > 0 ? '+' + str : str;
	}

	getTagPercentColor(tag: string) {
		return this.getTagPercentTargetDiffValue(tag) > 0 ? 'green' : 'red';
	}

	getTagPercentStyle(tag: string) {
		return `color: ${this.getTagPercentColor(tag)}`;
	}

	getAssetSum(sums: Record<string, SumStateDto>) {
		const sum = sums ? sums[this.currencyCode] : undefined;
		return sum ? sum.virtualSum : 0;
	}
}
</script>
