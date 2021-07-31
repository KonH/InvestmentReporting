<template>
	<h3>{{ currencyCode }}</h3>
	<table class="table table-sm table-striped">
		<thead>
			<tr>
				<th scope="col"></th>
				<template v-for="period in periods" :key="period">
					<th scope="col">{{ period }}</th>
				</template>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td><b>TOTAL</b></td>
				<template v-for="total in totals" :key="total.period">
					<td>
						<b><money :value="total.total" :currency-code="currencyCode" /></b>
					</td>
				</template>
			</tr>
			<template v-for="asset in targetAssets" :key="asset.id">
				<tr>
					<td>{{ asset.name }}</td>
					<template v-for="assetPeriod in assetPeriods.get(asset.id)" :key="assetPeriod.period">
						<td><money :value="assetPeriod.value" :currency-code="currencyCode" /></td>
					</template>
				</tr>
			</template>
		</tbody>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { OperationDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/common/money.vue';
import { VirtualAssetDto } from '@/api/market';

@Options({
	name: 'DividendCurrencyView',
	components: {
		Money,
	},
})
export default class DividendCurrencyView extends Vue {
	@Prop()
	currencyCode!: string;

	@Prop()
	operations!: OperationDto[];

	@Prop()
	assets!: VirtualAssetDto[];

	get periods() {
		return this.unique(this.operations.map((o) => o.date).map(this.formatPeriod));
	}

	formatPeriod(str: string | undefined) {
		if (!str) {
			return '';
		}
		const date = new Date(str);
		const month = (date.getMonth() + 1).toString().padStart(2, '0');
		const year = date.getFullYear();
		// MM/YYYY format
		return `${month}/${year}`;
	}

	get totals() {
		return this.periods.map((p) => {
			return { period: p, total: this.getPeriodTotal(p) };
		});
	}

	getPeriodTotal(period: string) {
		return this.assets
			.map((asset) => {
				const assetPeriods = this.assetPeriods.get(asset.id ?? '');
				return assetPeriods?.find((p) => p.period == period)?.value ?? 0;
			})
			.reduce((prev, current) => prev + current, 0);
	}

	get targetAssets() {
		const assetIds = this.operations.map((o) => o.asset).filter((assetId) => assetId);
		return this.unique(assetIds).map((assetId) => {
			const asset = this.assets.find((a) => a.id == assetId);
			return { id: assetId, name: asset?.name ?? '' };
		});
	}

	get assetPeriods() {
		const operationsByPeriod = new Map<string, OperationDto[]>();
		this.periods.forEach((p) => {
			operationsByPeriod.set(
				p,
				this.operations.filter((o) => this.formatPeriod(o.date) == p)
			);
		});
		const result = new Map<string, { period: string; value: number }[]>();
		this.targetAssets.forEach((asset) => {
			const assetDivs: { period: string; value: number }[] = [];
			this.periods.map((p) => {
				const periodOperations = operationsByPeriod.get(p);
				const assetPeriodDivs = periodOperations?.filter((o) => o.asset == asset.id) ?? [];
				const sum = assetPeriodDivs.map((o) => o.amount ?? 0).reduce((p, c) => p + c, 0);
				assetDivs.push({ period: p, value: sum });
			});
			result.set(asset.id, assetDivs);
		});
		return result;
	}

	unique<T>(source: T[]) {
		return Array.from(new Set<T>(source));
	}
}
</script>
