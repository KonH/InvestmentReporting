<template>
	<div>
		<b>Total:</b> <money :value="sum" :currency-id="currencyId" />
		<ul>
			<li v-for="tag in dashboard.tags" :key="tag.tag">
				<b>{{ tag.tag }}</b
				>: <money :value="getTagSum(tag.tag)" :currency-id="currencyId" /> <b>{{ getTagPercent(tag.tag) }}%</b>
			</li>
		</ul>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { DashboardStateDto } from '@/api/meta';
import Money from '@/component/money.vue';

@Options({
	name: 'DashboardLegend',
	components: {
		Money,
	},
})
export default class DashboardLegend extends Vue {
	@Prop()
	dashboard!: DashboardStateDto;

	@Prop()
	currencyId!: string;

	get sum() {
		const sums = this.dashboard.sums;
		return sums ? sums[this.currencyId].virtualSum : 0;
	}

	getTagSum(tag: string) {
		const tags = this.dashboard.tags;
		if (!tags) {
			return 0;
		}
		const sums = tags.find((t) => t.tag == tag)?.sums;
		return sums ? sums[this.currencyId].virtualSum : 0;
	}

	getTagPercent(tag: string) {
		const tagSum = this.getTagSum(tag) ?? 0;
		const sum = this.sum ?? 0;
		return ((tagSum / sum) * 100).toFixed(2);
	}
}
</script>
